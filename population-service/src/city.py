import qwikidata
import qwikidata.sparql
import requests
import json

class City(object): 
    def get_city_opendata(city, country):
        tmp = 'https://public.opendatasoft.com/api/records/1.0/search/?dataset=worldcitiespop&q=%s&sort=population&facet=country&refine.country=%s'
        cmd = tmp % (city, country)
        res = requests.get(cmd)
        dct = json.loads(res.content)
        out = dct['records'][0]['fields']
        return out
