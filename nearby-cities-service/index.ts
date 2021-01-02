import 'reflect-metadata';
import {Container} from './src/di/container';
import {container} from 'tsyringe';
import {GrpcServer} from './src/server';

new Container(container);

container.resolve(GrpcServer).start();
