import { Server, ServerCredentials } from 'grpc';
import { injectable, inject } from 'tsyringe';
import config from '../config.json';
import { ServiceDefinition } from './service/serviceDefinition';
import { Logger } from './util/logging/logger';
import { AutoDiscovery } from './discovery/autodiscovery';
import { Interceptor } from './interceptors/interceptor';
import { NearbyCitiesService } from './service/nearbycitiesService';

const interceptors = require('@echo-health/grpc-interceptors');

@injectable()
export class GrpcServer {
  constructor(@inject('NearbyCitiesService')
                    private nearbyCitiesService: NearbyCitiesService,
                @inject('AutoDiscovery')
                    private autoDiscovery: AutoDiscovery,
                @inject('Interceptor')
                    private loggerInterceptor: Interceptor,
                @inject('Logger')
                    private logger: Logger) {}

  start() : void {
    const host = process.env.HOST || config.host;
    const port = (process.env.PORT || config.port) as number;

    const cityinformation = ServiceDefinition.getCityInformation();
    const healthCheck = ServiceDefinition.getStatusPackage();

    const grpcServer = new Server();
    grpcServer.bind(`${host}:${port}`, ServerCredentials.createInsecure());


    const server = interceptors.serverProxy(grpcServer);
    server.use(this.loggerInterceptor.intercept);

    const service = this.nearbyCitiesService;
    server.addService(cityinformation.CityService.service,
      { getCityInformation: service.getCityInformation });
    
    server.addService(healthCheck.HealthCheckService.service,
      { getStatus: (call: any, callback: any) => { callback(null, { response: "pong"}); } });

    this.autoDiscovery.registerAutoDiscovery(server.handlers, port).then();

    server.start();

    this.logger.info(`Server running on ${host}:${port}`,
      { host, port });
  }
}
