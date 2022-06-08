import { inject, scoped, Lifecycle } from 'tsyringe';
import { Logger } from '../util/logging/logger';
import { Interceptor } from './interceptor';

@scoped(Lifecycle.ResolutionScoped)
export class LoggerInterceptor implements Interceptor {
  constructor(@inject('Logger') private logger: Logger) { }

    intercept = async (ctx: any, next: any, error: any) => {
      const metadata = ctx.call.metadata.internalRepr;

      const correlationId = metadata?.correlation_id ? metadata.correlation_id[0] : 'undefined';
      const rpc = metadata?.rpc ? metadata.rpc[0] : 'undefined';

      this.logger.info(`Request for ${rpc} STARTED. correlation-id: ${correlationId}`, metadata);

      try {
        await next();
        this.logger.info(`Request for ${rpc} FINISHED. correlation-id: ${correlationId}`, metadata);
      } catch {
        this.logger.error(`Request ${rpc} FAILED. correlation-id: ${correlationId}`, error);
      }
    }
}
