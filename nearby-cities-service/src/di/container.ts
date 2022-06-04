import { DependencyContainer } from 'tsyringe';
import { Logger } from '../util/logging/logger';
import { Interceptor } from '../interceptors/interceptor';
import { LoggerInterceptor } from '../interceptors/loggerInterceptor';
import { LegacyNearbyCitiesService } from '../service/legacyNearbycitiesService';
import { MixedLogger } from '../util/logging/mixedLogger';
import { NearbyCitiesService } from '../service/nearbycitiesService';
import { MetricsProvider } from '../util/metrics/metricsProvider';
import { MetricsInterceptor } from '../interceptors/metricsInterceptor';
import Prometheus from '../util/metrics/prometheus';

export class Container {
  constructor(container: DependencyContainer) {
    container.register<Logger>('Logger', {
      useClass: MixedLogger,
    });

    container.register<Interceptor>('Interceptor', {
      useClass: LoggerInterceptor,
    });

    container.register<NearbyCitiesService>('NearbyCitiesService', {
      useClass: LegacyNearbyCitiesService,
    });

    container.register<MetricsProvider>('Metrics', {
      useClass: Prometheus,
    });

    container.register<Interceptor>('MetricsInterceptor', {
      useClass: MetricsInterceptor,
    });
  }
}
