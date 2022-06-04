import { singleton } from 'tsyringe';
import { transports, createLogger, format} from 'winston';
import { Guid } from "typescript-guid";

import { Logger } from './logger';

@singleton()
export class MixedLogger implements Logger {
 private logger: Logger | undefined;

  constructor() {
    const fileName = (process.env.LOGGER_PATH || '/tmp/nearby-cities-{id}.log')
                        .replace("{id}", Guid.create().toString())

    this.logger = createLogger({
      format: format.combine(format.timestamp({alias: 'time'}), format.json()),
      transports: [
        new transports.Console(),
        new transports.File({ filename: fileName })
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
