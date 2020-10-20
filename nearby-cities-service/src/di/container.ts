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
import { Vault } from '../util/secret/vault';
import { HashicorpVault } from '../util/secret/hashicorpVault';

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

    container.register<Vault>('Vault', {
      useClass: HashicorpVault,
    });
  }
}
