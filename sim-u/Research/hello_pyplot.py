#!/usr/bin/env python3

# =============================
# Hello Python Plotting!
# https://matplotlib.org/users/pyplot_tutorial.html
# =============================

import numpy as np
import matplotlib.pyplot as plt

# Simple Plotting
plt.plot([1, 2, 5, 10], [-20, 13, 44, 6])
plt.show()

# Using numpy to generate an evenly distributed range
# Then pass into pyplot as functions on top of the numpy array (with special plotting arguments!)
t = np.arange(start=0, stop=5, step=0.2)
plt.plot(t, t, 'r--', t, t**2, 'bs', t, t**3, 'g^')
plt.show()

# Using a function that I've defined elsewhere
def f(t):
    return np.exp(-t) * np.cos(2*np.pi*t)

t1 = np.arange(start=0, stop=5, step=0.1)
t2 = np.arange(start=0, stop=5, step=0.02)

plt.subplot(211)
plt.plot(t1, f(t1), 'bo', t2, f(t2), 'k')

plt.subplot(212)
plt.plot(t2, np.cos(2*np.pi*t2), 'r--')
plt.show()
