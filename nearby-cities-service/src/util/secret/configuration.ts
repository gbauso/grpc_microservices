import { singleton } from 'tsyringe';
import config from '../../../config.json';
import { Secret } from './secret';

@singleton()
export class Configuration implements Secret {

  async getSecretValue(key: string): Promise<any> {
    const reference = Object.keys(config).indexOf(key.toLocaleLowerCase());
    const value = Object.entries(config)[reference];
    return Promise.resolve(value[1]);
  }
}
