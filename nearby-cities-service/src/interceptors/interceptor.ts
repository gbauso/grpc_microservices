export interface Interceptor {
    intercept : (ctx: any, next: any, error: any) => Promise<void>;
}
