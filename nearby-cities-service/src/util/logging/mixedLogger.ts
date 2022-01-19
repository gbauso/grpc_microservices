import { singleton } from 'tsyringe';
import { transports, createLogger, format} from 'winston';

import { Logger } from './logger';

@singleton()
export class MixedLogger implements Logger {
 private logger: Logger | undefined;

  constructor() {
    this.logger = createLogger({
      format: format.combine(format.timestamp({alias: 'time'}), format.json()),
      transports: [
        new transports.Console(),
        new transports.File({ filename: process.env.LOGGER_PATH || '/tmp/nearby-cities.log' })
      ]
    });
  }

  info(message: string, data?: any[] | undefined): void {
    this.logger?.info(message, data);
  }

  error(message: string, data?: any[] | undefined): void {
    this.logger?.error(message, data);
  }

  debug(message: string, data?: any[] | undefined): void {
    this.logger?.debug(message, data);
  }
}
