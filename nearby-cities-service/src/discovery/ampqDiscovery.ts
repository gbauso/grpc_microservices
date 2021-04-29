import { AutoDiscovery } from './autodiscovery';
import { Message } from './message';
import { singleton } from 'tsyringe';
import * as rabbitmq from 'amqplib';
import 'amqplib/callback_api';
import retry from 'async-retry';

@singleton()
export class AMPQDiscovery implements AutoDiscovery {
  constructor() {}

  private queue: string = 'discovery';

  private connection!: rabbitmq.Connection;

  private async connectToAmpq() {
    const credentials = {
      host: process.env.SB_HOST,
      port: process.env.SB_PORT,
      username: process.env.SB_USER,
      password: process.env.SB_PWD,
      protocol: process.env.SB_SSL ? "amqps" : "amqp",
      vhost: process.env.SB_SSL ? `/${process.env.SB_USER}` : ''
    }

    const host = `${credentials.host}:${credentials.port}/${credentials.vhost}`;
    const connectionString = `${credentials.protocol}://${credentials.username}:${credentials.password}@${host}`;
    this.connection = await retry<rabbitmq.Connection>(
      async () => await rabbitmq.connect(connectionString),
      { retries: 3 },
    );

    return await this.connection.createConfirmChannel();
  }

  async registerAutoDiscovery(handlers: any, port: number) {
    const channel = await this.connectToAmpq();
    await channel.assertQueue(this.queue);

    const message : Message = {
      handlers: this.getImplementedServices(handlers),
      service: `${process.env.REGISTER_AS}:${port}`,
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
