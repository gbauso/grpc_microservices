export interface Vault {
    getSecretValue(key: string) : Promise<any>
}
