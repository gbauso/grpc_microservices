import { Server, ServerCredentials } from 'grpc';
import { injectable, inject } from 'tsyringe';
import { ServiceDefinition } from './service/serviceDefinition';
import { Logger } from './util/logging/logger';
import { Interceptor } from './interceptors/interceptor';
import { NearbyCitiesService } from './service/nearbycitiesService';
import { serverProxy } from '@pionerlabs/grpc-interceptors';

@injectable()
export class GrpcServer {
  constructor(@inject('NearbyCitiesService')
                    private nearbyCitiesService: NearbyCitiesService,
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
      { GetCityInformation: service.getCityInformation });
    
    server.addService(healthCheck.HealthCheckService.service,
      { GetStatus: (call: any, callback: any) => { callback(null, { response: "pong"}) } });

    server.start();

    this.logger.info(`Server running on ${host}:${port}`,
      { host, port });
  }
}
