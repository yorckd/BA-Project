import cv2
import json
import socket
from ultralytics import YOLO
import torch

def start_server(host, port):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    server_socket.bind((host, port))
    print("UDP server up and listening...")
    return server_socket

def main():
    host = "localhost"
    port = int(input("Please enter the port number: "))

    client_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    #torch.cuda.set_device(0)
    model = YOLO('yolov8n-pose.pt')
    #model.to('cuda')

    video_path = 0
    cap = cv2.VideoCapture(video_path)

    while True:
        try:
            success, frame = cap.read()
            if success:
                results = model(frame, save=False)
                resultsKeypoints = results[0].keypoints.xyn.cpu().numpy()[0]
                message = json.dumps(resultsKeypoints.tolist())
                client_socket.sendto(message.encode(), (host, port))
            else:
                break
        except Exception as e:
            print(f"An unexpected error occurred: {e}")
            break

    cap.release()
    client_socket.close()

if __name__ == "__main__":
    main()
