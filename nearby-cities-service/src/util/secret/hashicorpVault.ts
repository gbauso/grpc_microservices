import NodeVault, { client } from 'node-vault';
import { singleton } from 'tsyringe';
import retry from 'async-retry';
import config from '../../../config.json';
import { Vault } from './vault';

@singleton()
export class HashicorpVault implements Vault {
  private vaultClient: client

  constructor() {
    this.vaultClient = NodeVault({
      token: process.env.VAULT_TOKEN || config.vault.token,
      endpoint: process.env.VAULT_HOST || config.vault.host,
    });
  }

  async getSecretValue<T>(key: string): Promise<T> {
    return await retry<T>(async () => {
      const keyValue = await this.vaultClient.read(key);
      return keyValue.data;
    }, { retries: 3 });
  }
}
