#!/usr/bin/env python3

# =============================
# Function used to generate an exponentially growing population size
# =============================

import numpy as np
import matplotlib.pyplot as plt
import random

def box_muller(mean, stdDev):
    u1 = 1.0 - random.random()
    u2 = 1.0 - random.random()
    randomStdNormal = np.sqrt(-2.0 * np.log(u1)) * np.cos(2 * np.pi * u2)
    return mean + (stdDev * randomStdNormal)

# Calculate histogram of values
minVal = 0
maxVal = 100

mean = 50
stdDev = 25

x = range(minVal, maxVal + 1, 1)
hist = [0] * (maxVal - minVal + 1)
for i in range(0, 100000, 1):
    rnd_val = box_muller(mean, stdDev)
    rnd_val_int = int(round(rnd_val))
    if (rnd_val_int >= minVal and rnd_val_int <= maxVal):
        hist[rnd_val_int - minVal] += 1
    else:
        print(f"Value {rnd_val_int} is outside range [{minVal},{maxVal}]")

plt.scatter(x, hist)
plt.show()
