import { AutoDiscovery } from './autodiscovery';
import { Message } from './message';
import { inject, singleton } from 'tsyringe';
import { Vault } from '../util/secret/vault';
import config from '../../config.json';
import * as rabbitmq from 'amqplib';
import 'amqplib/callback_api';
import retry from 'async-retry';

@singleton()
export class AMPQDiscovery implements AutoDiscovery {
  constructor(@inject('Vault') private vault: Vault) {}

  private queue: string = 'discovery';

  private connection!: rabbitmq.Connection;

  private async connectToAmpq() {
    const credentials = await this.vault.getSecretValue(config.bus.secret);

    const host = process.env.BUS_URL || config.bus.host;
    const connectionString = `amqp://${credentials.data.username}:${credentials.data.password}@${host}`;
    this.connection = await retry<rabbitmq.Connection>(
      async () => await rabbitmq.connect(connectionString),
      { retries: 10 },
    );

    return await this.connection.createConfirmChannel();
  }

  async registerAutoDiscovery(handlers: any, port: number) {
    const channel = await this.connectToAmpq();
    await channel.assertQueue(this.queue);

    const message : Message = {
      handlers: this.getImplementedServices(handlers),
      service: `${process.env.REGISTER_AS || config.register_as}:${port}`,
    };

    const json = JSON.stringify(message);

    channel.sendToQueue(this.queue, Buffer.from(json), { persistent: true });
    await channel.waitForConfirms();

    this.connection.close();
  }

  private getImplementedServices(handlers: any) : string[] | undefined {
    return Object.entries(handlers)
      .find((ele: any[]) => ele[0])
        ?.filter((e : any) => typeof e === 'string' && e !== '')
        ?.map((m : any) => m.split('/')[1] as string);
  }
}
