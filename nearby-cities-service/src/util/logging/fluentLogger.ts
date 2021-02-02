import fluentLogger, { FluentSender } from 'fluent-logger';
import { singleton } from 'tsyringe';
import { Logger } from './logger';
import { LogContent } from './logContent';

@singleton()
export class FluentLogger implements Logger {
    private logger: FluentSender<any>

    constructor() {
      this.logger = fluentLogger.createFluentSender<any>('nearby-cities', {
        host: process.env.LOGGER_HOST,
        port: (process.env.LOGGER_PORT || 0) as number,
        timeout: (process.env.LOGGER_TIMEOUT
          || 0) as number,
        requireAckResponse: (process.env.LOGGER_SYNC
          || false) as boolean,
        reconnectInterval: 1000,
      });
    }

    private log(level: string, message: string, data?: any) {
      const logContent : LogContent = {
        ...data,
        level,
        m: message,
      };
      this.logger.emit(logContent);
    }

    info(message: string, data?: any) : void {
      this.log('information', message, data);
    }

    error(message: string, data?: any) : void {
      this.log('error', message, data);
    }

    debug(message: string, data?: any) : void {
      this.log('debug', message, data);
    }
}
