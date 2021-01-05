import {status} from 'grpc';

export interface CallMetrics {
    responseTime: number
    statusCode: string
    method: string
    callType: string
}