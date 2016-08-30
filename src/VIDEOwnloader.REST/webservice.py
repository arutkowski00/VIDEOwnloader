#!/usr/bin/env python

import logging

from logging.handlers import RotatingFileHandler

from flask import Flask, jsonify
from flask_restful import Api, Resource, reqparse

from youtube_dl import YoutubeDL


logger = logging.getLogger(__name__)
logging.basicConfig(format="[%(asctime)s] (%(name)s) %(levelname)s: %(message)s")

app = Flask(__name__)
api = Api(app)

app.debug = True

ydl = YoutubeDL(params={
    'logger': app.logger,
    'extract_flat': 'in_playlist',
})

parser = reqparse.RequestParser()
parser.add_argument('url', action='append', required=True)


class VideoInfo(Resource):
    def post(self): #pylint: disable=R0201
        args = parser.parse_args()
        infos = [ydl.extract_info(url, download=False) for url in args['url']]
        playlists, videos = [], []
        for info in infos:
            # TODO: hunt for more _types
            info.pop('http_headers', None)
            if info.get('_type', '') == 'playlist':
                info.pop('_type', None)
                playlists.append(info)
            else:
                videos.append(info)
        return jsonify(playlists=playlists,
                       videos=videos)

class VideoUrlValid(Resource):
    def post(self): #pylint: disable=R0201
        args = parser.parse_args()
        valid_urls = []
        for url in args['url']:
            app.logger
            valid_urls.append({
                'url': url,
                'is_valid': any(ie.suitable(url) for ie in ydl._ies if ie.IE_NAME != 'generic')
            })
        return jsonify(valid=valid_urls)

api.add_resource(VideoInfo, '/videos')
api.add_resource(VideoUrlValid, '/valid')

if __name__ == '__main__':
    if app.debug is not True:
        file_handler = RotatingFileHandler('errors.log', maxBytes=1024 * 1024 * 100, backupCount=20)
        file_handler.setLevel(logging.ERROR)
        formatter = logging.Formatter("%(asctime)s - %(name)s - %(levelname)s - %(message)s")
        file_handler.setFormatter(formatter)
        app.logger.addHandler(file_handler)
    else:
        logger.setLevel(logging.DEBUG)
    app.run(host='0.0.0.0')
