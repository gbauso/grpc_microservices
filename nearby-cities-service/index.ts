import 'reflect-metadata';
import {Container} from './src/di/container';
import {container} from 'tsyringe';
import {GrpcServer} from './src/server';
import { MetricsProvider } from "./src/util/metrics/metricsProvider";
import os from 'node-os-utils';

new Container(container);

container.resolve(GrpcServer).start();

// const metrics = container.resolve<Promise<MetricsProvider>>('Metrics');

// setInterval(async () => {
//     (await metrics).collectServerMetrics({
//         cpuUsage: await os.cpu.usage(),
//         memoryFree: (await os.mem.free()).freeMemMb,
//         memoryUsage: (await os.mem.used()).usedMemMb
//     })
// }, 30000)