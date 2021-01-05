import { DependencyContainer } from 'tsyringe';
import { FluentLogger } from '../util/logging/fluentLogger';
import { AMPQDiscovery } from '../discovery/ampqDiscovery';
import { Logger } from '../util/logging/logger';
import { AutoDiscovery } from '../discovery/autodiscovery';
import { Interceptor } from '../interceptors/interceptor';
import { LoggerInterceptor } from '../interceptors/loggerInterceptor';
import { LegacyNearbyCitiesService } from '../service/legacyNearbycitiesService';
import { ConsoleLogger } from '../util/logging/consoleLogger';
import { NearbyCitiesService } from '../service/nearbycitiesService';
import { Secret } from '../util/secret/secret';
import { HashicorpVault } from '../util/secret/hashicorpVault';
import { MetricsProvider } from '../util/metrics/metricsProvider';
import { MetricsInterceptor } from '../interceptors/metricsInterceptor';
import { Configuration } from '../util/secret/configuration';
import Prometheus from '../util/metrics/prometheus';

export class Container {
  constructor(container: DependencyContainer) {
    container.register<Logger>('Logger', {
      useClass: process.env.DEBUG ? ConsoleLogger : FluentLogger,
    });

    container.register<AutoDiscovery>('AutoDiscovery', {
      useClass: AMPQDiscovery,
    });

    container.register<Interceptor>('Interceptor', {
      useClass: LoggerInterceptor,
    });

    container.register<NearbyCitiesService>('NearbyCitiesService', {
      useClass: LegacyNearbyCitiesService,
    });

    container.register<Secret>('Secret', {
      useClass: process.env.DEBUG ? Configuration : HashicorpVault,
    });

    container.register<MetricsProvider>('Metrics', {
      useClass: Prometheus,
    });

    container.register<Interceptor>('MetricsInterceptor', {
      useClass: MetricsInterceptor,
    });
  }
}
