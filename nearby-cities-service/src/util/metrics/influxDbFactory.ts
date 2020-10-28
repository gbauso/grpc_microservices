import { singleton, inject, FactoryProvider } from 'tsyringe';
import { Secret } from '../secret/secret';
import InfluxDBV2Metrics from './influxDbV2';
import InfluxDBMetrics from './influxDb';
import { MetricsProvider } from './metricsProvider';
import { DependencyContainer } from 'tsyringe';

export class InfluxDbFactory {
  
  static async getInstance(container: DependencyContainer): Promise<MetricsProvider> {
    const secret = container.resolve<Secret>("Secret"); 
    const credentials = await secret.getSecretValue('Metrics');

    return Promise.resolve<MetricsProvider>(
      credentials.token ?
        new InfluxDBV2Metrics(credentials) :
        new InfluxDBMetrics(credentials)
    );
  }
}