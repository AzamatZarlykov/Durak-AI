mport matplotlib.pyplot as plt

# This script is used just to draw the plots. It is not automated.
# The values has to be included manually.


def alpha_beta_graph():
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
	plt.ylabel("Average time per move (ms)")
	plt.legend()

	# Save the plot as a PNG image in the current working directory
	plt.savefig("../Figures/alpha-beta_comparison.png")

	# Show the plot
	plt.show()

def mcts_config_openworld():
	# List of values for the depth parameter
	depths = [2, 4, 6, 8, 10, 12]

	# Lists of values representing the win rate and average time taken to make a move at each depth
	win_rates = [626,498, 478, 550, 610, 688]
	times = [0.8021, 5.7641, 21.6235, 57.7265, 160.4083, 581.7359]

	# Create the main plot
	fig, ax = plt.subplots()

	# Plot the data for the win rates on the main y-axis
	ax.plot(depths, win_rates, label="Win rate", color='orange')

	# Create a secondary y-axis on the right side of the plot
	ax2 = ax.twinx()

	# Plot the data for the times on the secondary y-axis
	ax2.plot(depths, times, label="Average time per move (ms)", color='blue')

	# Add axis labels and a legend
	ax.set_xlabel("Depth")
	ax.set_ylabel("Win rate")
	ax2.set_ylabel("Average time per move (ms)")
	ax.legend()
	ax2.legend()

	# Show the plot
	plt.show()

#alpha_beta_graph()
mcts_config_openworld()