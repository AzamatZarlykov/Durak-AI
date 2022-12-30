import matplotlib.pyplot as plt

# Read the file and store the values in a dictionary
values = {}
with open('../ParamLogs/bf.txt') as f:
    for line in f:
        for pair in line.split():
            x, y = pair.split(':')
            values[int(y)] = int(x)

# Extract the x and y values from the dictionary
x = list(values.keys())
y = list(values.values())
print(f"X: {x}")
print(f"Y: {y}")

# Create the plot
plt.plot(x, y)

# Add a title and labels to the x and y axes
plt.title('Sample Graph')
plt.xlabel('Number of bouts')
plt.ylabel('Branching Factor')

# Save the plot to a file in a specific directory
plt.savefig('../Figures/branching_factor.png')