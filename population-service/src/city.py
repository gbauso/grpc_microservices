import qwikidata
import qwikidata.sparql
import requests
import json

class City(object): 
    def get_city_opendata(city, country):
        tmp = 'https://public.opendatasoft.com/api/records/1.0/search/?dataset=geonames-all-cities-with-a-population-1000&q=%s&sort=name&facet=feature_code&facet=cou_name_en&facet=timezone&refine.country_code=%s'
        cmd = tmp % (city, country.upper())
        res = requests.get(cmd)
        dct = json.loads(res.content)
        out = dct['records'][0]['fields']
        return out
