import json

import cv2
import numpy as np
import dlib
import requests

# ----------- LOGIN --------------------------------------------
url = 'http://192.168.0.105:3000/user/login'
loginData = {'username': 'aaa', 'password': 'aaa'}
x = requests.post(url, data=loginData)

data = json.loads(x.text)
cookie1 = data['jwt'][0:int(len(data['jwt'])/2)]
cookie2 = data['jwt'][int(len(data['jwt'])/2):len(data['jwt'])]
# --------------------------------------------------------------


cap = cv2.VideoCapture(0)

detector = dlib.get_frontal_face_detector()
predictor = dlib.shape_predictor("shape_predictor_68_face_landmarks.dat")

font = cv2.FONT_HERSHEY_COMPLEX


def midpoint(p1, p2):
    return int((p1.x + p2.x)/2), int((p1.y + p2.y)/2)


def eye_blink(facial_landmarks):
    left_point = (landmarks.part(36).x, landmarks.part(36).y)
    right_point = (landmarks.part(39).x, landmarks.part(39).y)
    center_top = midpoint(landmarks.part(37), landmarks.part(38))
    center_bottom = midpoint(landmarks.part(41), landmarks.part(40))

    # hor_line = cv2.line(frame, left_point, right_point, (0, 255, 0), 1)
    # ver_line = cv2.line(frame, center_top, center_bottom, (0, 255, 0), 1)
    l1 = cv2.line(frame, center_top, right_point, (0, 255, 0), 1)
    l2 = cv2.line(frame, center_top, left_point, (0, 255, 0), 1)
    l3 = cv2.line(frame, center_bottom, right_point, (0, 255, 0), 1)
    l4 = cv2.line(frame, center_bottom, left_point, (0, 255, 0), 1)

    hor_line_length = np.hypot((left_point[0] - right_point[0]), (left_point[1] - right_point[1]))
    ver_line_length = np.hypot((center_top[0] - center_bottom[0]), (center_top[1] - center_bottom[1]))

    ratio = hor_line_length / ver_line_length
    #print(ratio)

    if ratio > 6:
        cv2.putText(frame, "BLINKING", (50, 150), font, 1, (250, 0, 0))


def get_gaze_ratio(eye_points, facial_landmarks):
    count = 1
    ratio_avg = 0
    ratio_ver = -1
    for i in range(0, 10):
        left_eye_region = np.array([(facial_landmarks.part(eye_points[0]).x - 2, facial_landmarks.part(eye_points[0]).y),
                                    (facial_landmarks.part(eye_points[1]).x - 1, facial_landmarks.part(eye_points[1]).y + 1),
                                    (facial_landmarks.part(eye_points[2]).x + 1, facial_landmarks.part(eye_points[2]).y + 1),
                                    (facial_landmarks.part(eye_points[3]).x + 2, facial_landmarks.part(eye_points[3]).y),
                                    (facial_landmarks.part(eye_points[4]).x + 1, facial_landmarks.part(eye_points[4]).y - 1),
                                    (facial_landmarks.part(eye_points[5]).x - 1, facial_landmarks.part(eye_points[5]).y - 1)], np.int32)

        cv2.polylines(frame, [left_eye_region], True, (0, 0, 255), 2)

        # pozicija očesa (on black screen)
        height, width, _ = frame.shape
        mask = np.zeros((height, width), np.uint8)
        cv2.polylines(mask, [left_eye_region], True, 255, 1)
        cv2.fillPoly(mask, [left_eye_region], 255)
        eye = cv2.bitwise_and(gray, gray, mask=mask)

        min_x = np.min(left_eye_region[:, 0])
        max_x = np.max(left_eye_region[:, 0])
        min_y = np.min(left_eye_region[:, 1])
        max_y = np.max(left_eye_region[:, 1])

        gray_eye = eye[min_y: max_y, min_x: max_x]
        _, threshold_eye = cv2.threshold(gray_eye, 120, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)

        threshold_eye = cv2.resize(threshold_eye, None, fx=5, fy=5)
        cv2.imshow("Threshold", threshold_eye)
        # cv2.imshow("Eye", eye)
        # cv2.imshow("Left eye", left_eye)

        # levi in desni del očesa
        height_tr, width_tr = threshold_eye.shape
        left_side_threshold = threshold_eye[0: height_tr, 0: int(width_tr / 2)]
        left_side_white = cv2.countNonZero(left_side_threshold)
        right_side_threshold = threshold_eye[0: height_tr, int(width_tr / 2): width_tr]
        right_side_white = cv2.countNonZero(right_side_threshold)

        up_side_threshold = threshold_eye[int(height_tr/2): height_tr, 0: width_tr]
        up_side_white = cv2.countNonZero(up_side_threshold)
        down_side_threshold = threshold_eye[0: int(height_tr/2), 0: width_tr]
        down_side_white = cv2.countNonZero(down_side_threshold)

        if right_side_white != 0 and down_side_white != 0:
            ratio_hor = left_side_white / right_side_white
            ratio_ver = up_side_white / down_side_white
            ratio_avg = ratio_avg + ratio_hor
            count = count+1

    return ratio_avg/count, ratio_ver


while True:
    _, frame = cap.read()
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    faces = detector(gray)
    for face in faces:

        landmarks = predictor(gray, face)

        #eye_blink(landmarks)

        gaze_ratio_left_eye_hor, gaze_ratio_left_eye_ver = get_gaze_ratio([36, 37, 38, 39, 40, 41], landmarks)
        gaze_ratio_right_eye_hor, gaze_ratio_right_eye_ver = get_gaze_ratio([42, 43, 44, 45, 46, 47], landmarks)

        if gaze_ratio_left_eye_hor != -1 and gaze_ratio_right_eye_hor != -1:
            gaze_ratio_hor = (gaze_ratio_left_eye_hor + gaze_ratio_right_eye_hor)/2
            print(gaze_ratio_hor)

            if gaze_ratio_hor < 0.61:
                cv2.putText(frame, "RIGHT", (50, 100), font, 2, (0, 0, 255), 3)  # ----> JUMP
                url = 'http://192.168.0.105:3000/user/slide'
                cookies = {'token1': cookie1, 'token2': cookie2}
                x = requests.post(url, cookies=cookies)
                # print(x.text)
            elif gaze_ratio_hor > 1.55:
                cv2.putText(frame, "LEFT", (50, 100), font, 2, (0, 0, 255), 3)  # ----> SLIDE
                url = 'http://192.168.0.105:3000/user/jump'
                cookies = {'token1': cookie1, 'token2': cookie2}
                x = requests.post(url, cookies=cookies)
                # print(x.text)

    cv2.imshow("Frame", frame)
    key = cv2.waitKey(25)
    if key == 27:
        break

cap.release()
cv2.destroyAllWindows()
