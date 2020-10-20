import fluentLogger, { FluentSender } from 'fluent-logger';
import { singleton } from 'tsyringe';
import { Logger } from './logger';
import config from '../../../config.json';
import { LogContent } from './logContent';

@singleton()
export class FluentLogger implements Logger {
    private logger: FluentSender<any>

    constructor() {
      this.logger = fluentLogger.createFluentSender<any>('nearby-cities', {
        host: process.env.LOGGER_HOST || config.logger.host,
        port: (process.env.LOGGER_PORT || config.logger.port) as number,
        timeout: (process.env.LOGGER_TIMEOUT
          || config.logger.timeout) as number,
        requireAckResponse: (process.env.LOGGER_SYNC
          || config.logger.sync) as boolean,
        reconnectInterval: 1000,
      });
    }

    private log(level: string, message: string, data?: any[]) {
      const logContent : LogContent = {
        data,
        level,
        m: message,
      };
      this.logger.emit(logContent);
    }

    info(message: string, data?: Array<any>) : void {
      this.log('Information', message, data);
    }

    error(message: string, data?: Array<any>) : void {
      this.log('Error', message, data);
    }

    debug(message: string, data?: Array<any>) : void {
      this.log('Debug', message, data);
    }
}
