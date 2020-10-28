export interface Secret {
    getSecretValue(key: string) : Promise<any>
}
