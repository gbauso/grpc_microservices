import { singleton } from 'tsyringe';
import { Logger } from './logger';

@singleton()
export class ConsoleLogger implements Logger {
  info(message: string, data?: any[] | undefined): void {
    console.log(message, data);
  }

  error(message: string, data?: any[] | undefined): void {
    console.error(message, data);
  }

  debug(message: string, data?: any[] | undefined): void {
    console.debug(message, data);
  }
}
