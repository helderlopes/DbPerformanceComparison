import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

# ===== CONFIGURAÇÕES DE ESTILO =====
sns.set_theme(style="whitegrid")
plt.rcParams['figure.figsize'] = (10, 6)
plt.rcParams['axes.titlesize'] = 14
plt.rcParams['axes.labelsize'] = 12
plt.rcParams['legend.fontsize'] = 10

# ===== CARREGAR DADOS =====
df = pd.read_csv("metrics.csv")  # ajuste o caminho se necessário
df["ElapsedMs"] = df["ElapsedUs"] / 1000.0

# Define operações batch e single
batch_ops = ["AddManyAsync", "GetAllAsync", "DeleteAllAsync"]
single_ops = ["GetByIdAsync", "UpdateAsync", "DeleteAsync"]

for entity in df["EntityType"].unique():
    for operation in df["Operation"].unique():
        subset = df[(df["EntityType"] == entity) & (df["Operation"] == operation)]
        if subset.empty:
            continue

        plt.figure()

        if operation in batch_ops:
            # Gráfico de linha para batch
            sns.lineplot(
                data=subset,
                x="EntityCount",
                y="ElapsedMs",
                hue="Database",
                marker="o"
            )
            plt.xlabel("Quantidade de Entidades")

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
        plt.title(f"{entity} - {operation} - Tempo vs Escala")
        plt.grid(True, which="both", linestyle="--", linewidth=0.5)
        plt.ylim(bottom=0)
        plt.ylabel("Tempo (ms)")
        plt.legend(title="Banco de Dados")
        plt.tight_layout()
        plt.savefig(f"{entity}_{operation}.png", dpi=300)
        plt.close()

