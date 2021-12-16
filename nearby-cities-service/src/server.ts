import { Server, ServerCredentials } from '@grpc/grpc-js';
import { injectable, inject } from 'tsyringe';
import { ServiceDefinition } from './service/serviceDefinition';
import { Logger } from './util/logging/logger';
import { Interceptor } from './interceptors/interceptor';
import { NearbyCitiesService } from './service/nearbycitiesService';
import wrapServerWithReflection from 'grpc-node-server-reflection';
import { serverProxy } from '@speedymonster/grpc-interceptors'

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

    const grpcServer = wrapServerWithReflection(new Server());
    const service = this.nearbyCitiesService;
    grpcServer.addService(cityinformation.CityService.service,
      { GetCityInformation: service.getCityInformation });
    
    grpcServer.addService(healthCheck.HealthCheckService.service,
      { GetStatus: (call: any, callback: any) => { callback(null, { response: "pong"}) } });

    grpcServer.bindAsync(`${host}:${port}`, 
              ServerCredentials.createInsecure(),
              () => {
                server.start()

                this.logger.info(`Server running on ${host}:${port}`,
                { host, port });
              },);

    const server = serverProxy(grpcServer);
    server.use(this.loggerInterceptor.intercept);
    server.use(this.metricsInterceptor.intercept);

  }
}
