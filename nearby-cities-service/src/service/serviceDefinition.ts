import * as protoLoader from '@grpc/proto-loader';
import {loadPackageDefinition} from '@grpc/grpc-js';
import * as path from 'path';

export class ServiceDefinition {
  static getCityInformation(): any {
    const protoPath = path.join(__dirname,
        '../../contract/cityinformation.proto');

    const packageDefinition = protoLoader.loadSync(
        protoPath,
        {
          keepCase: true,
          longs: String,
          enums: String,
          defaults: true,
          oneofs: true,
        });

    return loadPackageDefinition(packageDefinition).cityinformation;
  }

  static getStatusPackage(): any {
    const protoPath = path.join(__dirname,
        '../../contract/healthcheck.proto');

    const packageDefinition = protoLoader.loadSync(
        protoPath,
        {
          keepCase: true,
          longs: String,
          enums: String,
          defaults: true,
          oneofs: true,
        });

    return loadPackageDefinition(packageDefinition).healthcheck;
  }
}
