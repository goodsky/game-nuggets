#!/usr/bin/env python3

# =============================
# Function used to generate a normally distributed population of student academic scores.
# =============================

import numpy as np
import matplotlib.pyplot as plt

def hello_normal():
    mu, sigma = 1060, 195 # mean and standard deviation
    s = np.random.normal(mu, sigma, size=1000)

    # Verify the mean and variance
    print("samples={}".format(len(s)))
    print("mu={}; mean={}".format(mu, np.mean(s)))
    print("sigma={}; std={}".format(sigma, np.std(s)))

    count, bins, ignored = plt.hist(s, bins=30, density=True)
    plt.plot(bins, 1/(sigma * np.sqrt(2 * np.pi)) *
                    np.exp( - (bins - mu)**2 / (2 * sigma**2)),
                    linewidth=2, color='r')

    valuesInStdDev = [p for p in s if p > mu - sigma and p < mu + sigma]
    print("% within StdDev = {}".format(len(valuesInStdDev) / len(s)))

    plt.show()

def generate_histogram(x, mean, variance, populationSize):
    # Shape = Gaussian bell curve
    return populationSize / (variance * np.sqrt(2 * np.pi)) * np.exp( -(x - mean)**2 / (2 * variance**2))

def convert_academic_score_to_sat(x):
    # linear conversion from academic score to SAT score
    # from the range 60 -> 100. (to 700 -> 1600)
    minAS = 60
    maxAS = 100
    minSAT = 800
    maxSAT = 1600
    return np.where(x <= 100,
        ((x - minAS) / (maxAS - minAS)) * (maxSAT - minSAT) + minSAT,
        maxSAT)

def convert_prestige_to_mean(x):
    # linear conversion from academic prestige to mean academic score
    minPrestige = 0
    maxPrestige = 100
    minAS = 60
    maxAS = 110
    return ((x - minPrestige) / (maxPrestige - minPrestige)) * (maxAS - minAS) + minAS

x_as = np.arange(start=60, stop=110, step=1)
x_prestige = np.arange(start=0, stop=100, step=1)

plt.subplot(311)
plt.plot(x_as, generate_histogram(x_as, 75, 5, 100))

plt.subplot(312)
plt.plot(x_as, convert_academic_score_to_sat(x_as))

plt.subplot(313)
plt.plot(x_prestige, convert_prestige_to_mean(x_prestige))

plt.show()