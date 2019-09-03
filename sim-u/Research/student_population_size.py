#!/usr/bin/env python3

# =============================
# Function used to generate an exponentially growing population size
# =============================

import numpy as np
import matplotlib.pyplot as plt

def population_size(x):
    # Shape = Exponential
    # Min Value = 100 students
    # Max Value = 10,000 students
    # X-Range = [0, 100]
    return exponential_mapping(x, 0, 100, 100, 10000, 4)

def tuition_bonus(x):
    # Shape = unbounded linear negative / bounded sigmoid positive
    # MinValue = -infinity
    # MaxValue = + 10
    # X-Range = [-infinity, +infinity (levels out around 5k)]
    
    return np.where(x >= 0,
        20 / (1 + np.exp(-x/2500)) - 10,
        (x / 500))

def target_tuition(x):
    # Shape = Exponential
    # Min Value = $2k / year
    # Max Value = $50k / year
    # X-Range = [0, 200]
    return exponential_mapping(x, 0, 200, 2000, 50000, 2.1)

def damping_mapping(x):
    scaledX = (x / 2000)
    return scaledX / (1 + np.abs(scaledX))

def exponential_mapping(x, minInput, maxInput, minOutput, maxOutput, exponent):
    maxValue = (maxInput - minInput)**exponent
    slope = (maxOutput - minOutput) / maxValue
    return ((x - minInput)**exponent * slope) + minOutput

x0 = np.arange(start=0, stop=100, step=1)
x1 = np.arange(start=-5000, stop=10000, step=10)
x2 = np.arange(start=0, stop=200, step=1)

plt.subplot(221)
plt.plot(x0, population_size(x0))

plt.subplot(222)
plt.plot(x1, tuition_bonus(x1))

plt.subplot(223)
plt.plot(x2, target_tuition(x2))

t = np.arange(-10, 60, 1)
plt.subplot(224)
# plt.plot(t, exponential_mapping(t, -10, 60, -12, 37, 1.5))
plt.plot(x1, damping_mapping(x1))

plt.show()
