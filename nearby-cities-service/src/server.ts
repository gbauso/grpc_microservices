import { Server, ServerCredentials } from '@grpc/grpc-js';
import { injectable, inject } from 'tsyringe';
import { ServiceDefinition } from './service/serviceDefinition';
import { Logger } from './util/logging/logger';
import { Interceptor } from './interceptors/interceptor';
import { NearbyCitiesService } from './service/nearbycitiesService';
import wrapServerWithReflection from 'grpc-node-server-reflection';
import { serverProxy } from '@speedymonster/grpc-interceptors'
import { HealthImplementation, service as svc, ServingStatus } from '@zcong/node-grpc-health-check'



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
    const host = process.env.HOST || "0.0.0.0";
    const port = (process.env.PORT || 50060) as number;

    const cityinformation = ServiceDefinition.getCityInformation();

    const service = this.nearbyCitiesService;

    const grpcServer = wrapServerWithReflection(new Server());
    const server = serverProxy(grpcServer);
    server.use(this.loggerInterceptor.intercept);
    server.use(this.metricsInterceptor.intercept);

    server.addService(cityinformation.CityService.service,
      { GetCityInformation: service.getCityInformation });
    
    const implementations = new HealthImplementation({
      'cityinformation.CityService': ServingStatus.SERVING,
      'grpc.health.v1.Health': ServingStatus.SERVING,
      'grpc.reflection.v1alpha.ServerReflection': ServingStatus.SERVING
    })

    server.addService(svc, implementations);
    
    server.bindAsync(`${host}:${port}`, 
              ServerCredentials.createInsecure(),
              () => {
                server.start()

                this.logger.info(`Server running on ${host}:${port}`,
                { host, port });
              },);
    
    process.on('exit', (code) => {
      implementations.shutdown()
    });

  }
}
