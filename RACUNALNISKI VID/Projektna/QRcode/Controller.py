from flask import Flask, render_template, request, jsonify
from flask_uploads import UploadSet, configure_uploads, IMAGES
import pyqrcode
from pyzbar.pyzbar import decode
from PIL import Image
from flask_cors import CORS, cross_origin

app = Flask(__name__)
CORS(app)

photos = UploadSet('photos', IMAGES)

app.config['UPLOADED_PHOTOS_DEST'] = 'static/img'
configure_uploads(app, photos)

equipment = [
    {
        'value': '1',
        'helmet': 'helmet_1',
        'armor': 'armor_1',
        'sword': 'sword_1',
        'boots': 'boots_1'
    },
    {
        'value': '2',
        'helmet': 'helmet_2',
        'armor': 'armor_2',
        'sword': 'sword_2',
        'boots': 'boots_2'
    },
    {
        'value': '3',
        'helmet': 'helmet_3',
        'armor': 'armor_3',
        'sword': 'sword_3',
        'boots': 'boots_3'
    }
]

# ------------------- PROFILE VIEW
@app.route('/')
def login():
    return render_template('login.html')


# ------------------- PROFILE VIEW
@app.route('/Profile')
def profile():
    return render_template('profile.html', equipment=equipment)

# ------------------- STORE VIEW
@app.route('/Store')
def store():
    return render_template('store.html')

# ------------------- GENERATE QR_CODE
@app.route('/GenerateQR', methods=['POST'])
def get_qr():
    s = request.form['value']

    filename = 'QR_' + s + '.png'
    qr = pyqrcode.create(s)
    qr.png('static/qr/'+filename, scale=8)
    if s:
        return jsonify({'src': filename})
    else:
        return jsonify({'error': 'missing data'})

# ------------------- UPLOAD IMAGE - DECODE QR
@app.route('/Upload', methods=['GET', 'POST'])
def upload():
    if request.method == 'POST' and request.files.get('photo'):
        try:
            filename = photos.save(request.files.get('photo'))
            d = decode(Image.open('static/img/' + filename))
            return d[0].data.decode('ascii')
        except:
            return "not valid image"
    return render_template('store.html')


if __name__ == '__main__':
    app.run(host='0.0.0.0')
