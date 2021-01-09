import 'reflect-metadata';
import {Container} from './src/di/container';
import {container} from 'tsyringe';
import {GrpcServer} from './src/server';
import dotenv from 'dotenv'

dotenv.config();
new Container(container);

container.resolve(GrpcServer).start();
