import NodeVault, { client } from 'node-vault';
import { singleton } from 'tsyringe';
import retry from 'async-retry';
import config from '../../../config.json';
import { Secret } from './secret';

@singleton()
export class HashicorpVault implements Secret {
  private vaultClient: client
  private prefixSecrets = 'kv/data'

  constructor() {
    this.vaultClient = NodeVault({
      token: process.env.VAULT_TOKEN || config.vault.token,
      endpoint: process.env.VAULT_HOST || config.vault.host,
    });
  }

  async getSecretValue<T>(key: string): Promise<T> {
    return await retry<T>(async () => {
      const keyValue = await this.vaultClient.read(`${this.prefixSecrets}/${key}`);
      return keyValue.data.data;
    }, { retries: 3 });
  }
}
