import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Definir estilo
sns.set_theme(style="whitegrid")
plt.rcParams['figure.figsize'] = (10, 6)

df = pd.read_csv("metrics.csv")  # ajuste o caminho se necessário
df["ElapsedMs"] = df["ElapsedUs"] / 1000.0

# Lista de operações
operations = df["Operation"].unique()

batch_ops = ["AddManyAsync", "GetAllAsync", "DeleteAllAsync"]
single_ops = ["GetByIdAsync", "UpdateAsync", "DeleteAsync"]

# Loop para cada operação -> gráfico por escala, agregando entidades
for operation in operations:
    subset = df[df["Operation"] == operation]
    if subset.empty:
        continue
    
    plt.figure()

    if operation in batch_ops:
        # Gráfico de linha para batch
        sns.lineplot(
            data=subset,
            x="Scale",
            y="ElapsedMs",
            hue="Database",
            marker="o"
        )
        plt.xlabel("Escala / Tamanho da Base")

    elif operation in single_ops:
        # Gráfico de boxplot para single
        sns.boxplot(
            data=subset,
            x="Scale",
            y="ElapsedMs",
            hue="Database"
        )
        plt.xlabel("Escala / Tamanho da Base")

    if operation == "DeleteAllAsync":
        plt.yscale("log")

    plt.title(f"Operação: {operation} - Tempo médio por escala (todas as entidades)")
    plt.xlabel("Escala (multiplicador de dados)")
    plt.ylabel("Tempo médio (ms)")
    plt.grid(True, linestyle="--", linewidth=0.5)
    plt.legend(title="Banco de Dados")
    plt.tight_layout()
    plt.savefig(f"{operation}_all_entities.png", dpi=300)
    plt.close()
