export interface AutoDiscovery {
    registerAutoDiscovery(handlers: any, port: number) : Promise<void>
}
