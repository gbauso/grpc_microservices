// legacy
const nearbyCities = require('nearby-cities');
import { scoped, Lifecycle} from 'tsyringe';
import {NearbyCitiesService} from './nearbycitiesService';

@scoped(Lifecycle.ContainerScoped)
export class LegacyNearbyCitiesService implements NearbyCitiesService {
  constructor() {}

  getCityInformation(call: any, callback: any) {
    const query = {latitude: call.request.lat, longitude: call.request.lon};

    const cities = nearbyCities(query);

    callback(null,
        {
          nearbyCities: cities.slice(1, 6).map((el: any) => el.name).join(';')
        }
    );
  }
}
