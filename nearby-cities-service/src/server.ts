import { Server, ServerCredentials } from 'grpc';
import { injectable, inject } from 'tsyringe';
import { ServiceDefinition } from './service/serviceDefinition';
import { Logger } from './util/logging/logger';
import { AutoDiscovery } from './discovery/autodiscovery';
import { Interceptor } from './interceptors/interceptor';
import { NearbyCitiesService } from './service/nearbycitiesService';
import { serverProxy } from '@pionerlabs/grpc-interceptors';

@injectable()
export class GrpcServer {
  constructor(@inject('NearbyCitiesService')
                    private nearbyCitiesService: NearbyCitiesService,
                @inject('AutoDiscovery')
                    private autoDiscovery: AutoDiscovery,
                @inject('Interceptor')
                    private loggerInterceptor: Interceptor,
                @inject('MetricsInterceptor')
                    private metricsInterceptor: Interceptor,
                @inject('Logger')
                    private logger: Logger) {}

  start() : void {
    const host = process.env.HOST;
    const port = (process.env.PORT || 0) as number;

    const cityinformation = ServiceDefinition.getCityInformation();
    const healthCheck = ServiceDefinition.getStatusPackage();

    const grpcServer:any = new Server();
    grpcServer.bind(`${host}:${port}`, ServerCredentials.createInsecure());


    const server = serverProxy(grpcServer);
    server.use(this.loggerInterceptor.intercept);
    server.use(this.metricsInterceptor.intercept);

    const service = this.nearbyCitiesService;
    server.addService(cityinformation.CityService.service,
      { getCityInformation: service.getCityInformation });
    
    server.addService(healthCheck.HealthCheckService.service,
      { getStatus: (call: any, callback: any) => { callback(null, { response: "pong"}); } });

    this.autoDiscovery.registerAutoDiscovery(grpcServer.handlers, port).then();

    server.start();

    this.logger.info(`Server running on ${host}:${port}`,
      { host, port });
  }
}
