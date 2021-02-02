import { inject, scoped, Lifecycle } from 'tsyringe';
import { Logger } from '../util/logging/logger';
import { Interceptor } from './interceptor';

@scoped(Lifecycle.ResolutionScoped)
export class LoggerInterceptor implements Interceptor {
  constructor(@inject('Logger') private logger: Logger) { }

    intercept = async (ctx: any, next: any, error: any) => {
      const metadata = ctx.call.metadata._internal_repr;

      const service = metadata?.service ? metadata.service[0] : 'undefined';
      const rpc = metadata?.rpc ? metadata.rpc[0] : 'undefined';

      this.logger.info(`Request for /${service}/${rpc} STARTED`, metadata);

      try {
        await next();
        this.logger.info(`Request for /${service}/${rpc} FINISHED`, metadata);
      } catch (err) {
        this.logger.error(`Request /${service}/${rpc} FAILED`, err.stack);
      }
    }
}
