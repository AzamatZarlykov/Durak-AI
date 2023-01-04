import matplotlib.pyplot as plt

# This script is used just to draw the plots. It is not automated.
# The values has to be included manually.

# List of values for the depth parameter
depths = [1, 2, 3, 4, 5, 6]

# Lists of values representing the average time taken to make a move by each system
system1_times = [0.4675, 0.8932, 1.5182, 2.9174, 5.5492, 10.5764]
system2_times = [0.4883, 1.4212, 3.3980, 27.4948, 50.9900, 258.1767]
# Create a bar plot showing the performance of both systems
plt.plot(depths, system1_times, label="with alpha-beta")
plt.plot(depths, system2_times, label="without alpha-beta")

# Add axis labels and a legend
plt.xlabel("Depth")
plt.ylabel("Average time per move (s)")
plt.legend()

# Save the plot as a PNG image in the current working directory
plt.savefig("../Figures/alpha-beta_comparison.png")

# Show the plot
plt.show()