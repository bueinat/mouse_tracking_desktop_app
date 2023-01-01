import concurrent.futures
import math

from utilityFunctions import *

# suppress all warnings
import warnings
warnings.filterwarnings("ignore")

PRIMES = [
    112272535095293,
    112582705942171,
    112272535095293,
    115280095190773,
    115797848077099,
    1099726899285419]

FILES = ["odor 1", "odor 13"]

def is_prime(n):
    if n < 2:
        return False
    if n == 2:
        return True
    if n % 2 == 0:
        return False

    sqrt_n = int(math.floor(math.sqrt(n)))
    for i in range(3, sqrt_n + 1, 2):
        if n % i == 0:
            return False
    return True

def parallel_function(filename):
    nframes = extract_video(args, video_name, frames_path)
    dscript_detect = run_inference(frames_path, nframes)
    uploadable_data = process_inference(dscript_detect, frames_path)
    return uploadable_data

def path_main():
    d = {}
    with concurrent.futures.ProcessPoolExecutor() as executor:
        for filename, data in zip(FILES, executor.map(parallel_function, FILES)):
            d[filename] = uploadable_data
            print('%d is prime: %s' % (number, prime))
                # copy video to cache and extract frames
            nframes = extract_video(args, video_name, frames_path)
            dscript_detect = run_inference(frames_path, nframes)
            uploadable_data = process_inference(dscript_detect, frames_path)
