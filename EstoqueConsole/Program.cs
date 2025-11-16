
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// --- Modelos simples ---
public record Produto(int Id, string Nome, string Categoria, int EstoqueMinimo, int Saldo);
public record Movimento(int Id, int ProdutoId, string Tipo, int Quantidade, DateTime Data, string Observacao);

class Program
{
    static string dataFolder = "data";
    static List<Produto> produtos = new();
    static List<Movimento> movimentos = new();
    static int nextProdutoId = 1;
    static int nextMovimentoId = 1;

    static void Main()
    {
        // Garante existência dos arquivos/pasta
        StorageInitializer.EnsureDataFiles(dataFolder);

        // Carrega (estubs): aqui você implementaria leitura CSV
        LoadProdutosFromCsv();


        // LoadMovimentosFromCsv();

        // Menu principal
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Clear();
            Console.WriteLine("=== CONTROLE DE ESTOQUE ===");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("1. Listar produtos");
            Console.WriteLine("2. Cadastrar produto");
            Console.WriteLine("3. Editar produto");
            Console.WriteLine("4. Excluir produto");
            Console.WriteLine("5. Dar ENTRADA em estoque");
            Console.WriteLine("6. Dar SAÍDA de estoque");
            Console.WriteLine("7. Relatório: Estoque abaixo do mínimo");
            Console.WriteLine("8. Relatório: Extrato de movimentos por produto");
            Console.WriteLine("9. Salvar (CSV)");
            Console.WriteLine("0. Sair");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Escolha uma opção: ");
            var op = Console.ReadLine();

            switch (op)
            {
                case "1":
                    ListarProdutos(); Pause();
                    break;
                case "2":
                    CadastrarProduto(); Pause();
                    break;
                case "3":
                    EditarProduto(); Pause();
                    break;
                case "4":
                    ExcluirProduto(); Pause();
                    break;
                case "5":
                    MovimentacaoEntrada(); Pause();
                    break;
                case "6":
                    MovimentacaoSaida(); Pause();
                    break;
                case "7":
                    RelatorioAbaixoMinimo(); Pause();
                    break;
                case "8":
                    ExtratoPorProduto(); Pause();
                    break;
                case "9":
                    SalvarTudo(); Pause();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida."); Pause();
                    break;
            }
        }
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.Write("Pressione Enter para continuar...");
        Console.ReadLine();
    }

    // --- Funções (stubs/implementações simples) ---
    static void ListarProdutos()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("== Lista de Produtos ==");
        if (!produtos.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(Nenhum produto cadastrado)");
            return;
        }
        foreach (var p in produtos)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{p.Id} - {p.Nome} | Cat: {p.Categoria} | Saldo: {p.Saldo} | Mínimo: {p.EstoqueMinimo}");
        }
    }

    static void CadastrarProduto()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("== Cadastrar Produto ==");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Nome: ");
        var nome = Console.ReadLine() ?? "";
        Console.Write("Categoria: ");
        var cat = Console.ReadLine() ?? "";
        Console.Write("Estoque mínimo (numero): ");
        if (!int.TryParse(Console.ReadLine(), out int minimo) || minimo < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Estoque mínimo inválido.");
            return;
        }

        var produto = new Produto(nextProdutoId++, nome, cat, minimo, 0);
        produtos.Add(produto);
        Console.WriteLine("Produto cadastrado com sucesso!");
    }

    static void EditarProduto()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Id do produto a editar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Id inválido.");
            return;
        }
        var p = produtos.FirstOrDefault(x => x.Id == id);
        if (p == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Produto não encontrado.");
            return;
        }

        Console.Write($"Nome ({p.Nome}): ");
        var nome = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(nome)) nome = p.Nome;

        Console.Write($"Categoria ({p.Categoria}): ");

        var cat = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cat)) cat = p.Categoria;

        Console.Write($"Estoque mínimo ({p.EstoqueMinimo}): ");

        var emStr = Console.ReadLine();
        int em = p.EstoqueMinimo;
        if (!string.IsNullOrWhiteSpace(emStr) && (!int.TryParse(emStr, out em) || em < 0))

        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Estoque mínimo inválido.");
            return;
        }

        produtos.Remove(p);
        produtos.Add(new Produto(p.Id, nome!, cat!, em, p.Saldo));
        Console.WriteLine("Produto atualizado.");
    }

    static void ExcluirProduto()
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.Write("Id do produto a excluir: ");
        if (!int.TryParse(Console.ReadLine(), out int id))

        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Id inválido.");

            return;
        }
        var p = produtos.FirstOrDefault(x => x.Id == id);
        if (p == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Produto não encontrado.");

            return;
        }

        if (p.Saldo < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Não é permitido excluir produto com saldo negativo.");

            return;
        }
        produtos.Remove(p);

        Console.WriteLine("Produto excluído.");
    }

    static void MovimentacaoEntrada()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Id do produto (entrada): ");
        if (!int.TryParse(Console.ReadLine(), out int id))

        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Id inválido.");

            return;
        }
        var p = produtos.FirstOrDefault(x => x.Id == id);

        if (p == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Produto não encontrado.");

            return;
        }

        Console.Write("Quantidade: ");
        if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd <= 0)

        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Quantidade inválida.");

            return;
        }

        Console.Write("Observação (opcional): ");
        var obs = Console.ReadLine() ?? "";

        // atualiza saldo (imutável -> recria registro)
        produtos.Remove(p);
        produtos.Add(new Produto(p.Id, p.Nome, p.Categoria, p.EstoqueMinimo, p.Saldo + qtd));

        var mov = new Movimento(nextMovimentoId++, id, "ENTRADA", qtd, DateTime.Now, obs);
        movimentos.Add(mov);
        Console.WriteLine("Entrada registrada.");
    }

    static void MovimentacaoSaida()
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.Write("Id do produto (saída): ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Id inválido.");
            return;
        }
        var p = produtos.FirstOrDefault(x => x.Id == id);
        if (p == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Produto não encontrado.");
            return;
        }

        Console.Write("Quantidade: ");
        if (!int.TryParse(Console.ReadLine(), out int qtd) || qtd <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Quantidade inválida.");
            return;
        }

        if (p.Saldo < qtd)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Saldo insuficiente. Operação cancelada.");

            return;
        }

        Console.Write("Observação (opcional): ");
        var obs = Console.ReadLine() ?? "";

        produtos.Remove(p);
        produtos.Add(new Produto(p.Id, p.Nome, p.Categoria, p.EstoqueMinimo, p.Saldo - qtd));

        var mov = new Movimento(nextMovimentoId++, id, "SAIDA", qtd, DateTime.Now, obs);
        movimentos.Add(mov);
        Console.WriteLine("Saída registrada.");
    }

    static void RelatorioAbaixoMinimo()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("== Produtos abaixo do mínimo ==");
        var abaixo = produtos.Where(p => p.Saldo < p.EstoqueMinimo).ToList();
        if (!abaixo.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(Nenhum produto abaixo do mínimo)");
            return;
        }

        foreach (var p in abaixo)
            Console.WriteLine($"{p.Id} - {p.Nome} | Saldo: {p.Saldo} | Mínimo: {p.EstoqueMinimo}");
    }

    static void ExtratoPorProduto()
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Console.Write("Id do produto para extrato: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Id inválido.");
            return;
        }

        var list = movimentos.Where(m => m.ProdutoId == id).OrderBy(m => m.Data).ToList();

        if (!list.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(Sem movimentos)");
            return;
        }

        foreach (var m in list)
            Console.WriteLine($"{m.Data:yyyy-MM-dd HH:mm} | {m.Tipo} | {m.Quantidade} | {m.Observacao}");
    }

    static void SalvarTudo()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        // Exemplo: salva apenas produtos agora (implemente movimentos da mesma forma)
        var produtosPath = Path.Combine(dataFolder, "produtos.csv");
        var movimentoPath = Path.Combine(dataFolder, "movimentos.csv");

        try
        {
            CsvStorage.SaveProdutosAtomic(produtosPath, produtos);
            CsvStorage.SaveMovimentoAtomic(movimentoPath, movimentos);
            Console.WriteLine("Produtos e movimentos salvos com sucesso.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Erro ao salvar: " + ex.Message);
        }
    }
    static void LoadProdutosFromCsv()
    {
        Console.ForegroundColor = ConsoleColor.Green;

        var produtosPath = Path.Combine(dataFolder, "produtos.csv");

        if (!File.Exists(produtosPath))
            return; // se não existir, não faz nada

        var linhas = File.ReadAllLines(produtosPath, Encoding.UTF8).Skip(1); // pula o cabeçalho

        produtos.Clear();

        foreach (var linha in linhas)
        {
            if (string.IsNullOrWhiteSpace(linha)) continue;

            var campos = linha.Split(';');
            if (campos.Length < 5) continue;

            int id = int.Parse(campos[0]);
            string nome = campos[1];
            string categoria = campos[2];
            int estoqueMinimo = int.Parse(campos[3]);
            int saldo = int.Parse(campos[4]);

            produtos.Add(new Produto(id, nome, categoria, estoqueMinimo, saldo));
        }

        // atualiza o próximo ID automaticamente
        if (produtos.Any())
            nextProdutoId = produtos.Max(p => p.Id) + 1;
    }

}

// --- Inicializador já mostrado — coloque isso em outro arquivo se preferir ---
public static class StorageInitializer
{
    public static void EnsureDataFiles(string basePath)
    {
        Console.ForegroundColor = ConsoleColor.Green;

        Directory.CreateDirectory(basePath);

        string produtosPath = Path.Combine(basePath, "produtos.csv");
        string movimentosPath = Path.Combine(basePath, "movimentos.csv");

        if (!File.Exists(produtosPath))
            File.WriteAllText(produtosPath, "id;nome;categoria;estoqueMinimo;saldo" + Environment.NewLine, new UTF8Encoding(false));

        if (!File.Exists(movimentosPath))
            File.WriteAllText(movimentosPath, "id;produtoId;tipo;quantidade;data;observacao" + Environment.NewLine, new UTF8Encoding(false));
    }
}

// --- Classe para salvar CSV de forma simples/atômica ---
public static class CsvStorage
{
    public static void SaveProdutosAtomic(string path, IEnumerable<Produto> produtos)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        var dir = Path.GetDirectoryName(path) ?? ".";
        Directory.CreateDirectory(dir);

        string temp = Path.Combine(dir, Path.GetRandomFileName() + ".tmp");

        using (var sw = new StreamWriter(temp, false, new UTF8Encoding(false)))
        {
            sw.WriteLine("id;nome;categoria;estoqueMinimo;saldo");
            foreach (var p in produtos.OrderBy(p => p.Id))
            {
                // Escape simples: substitui ; dentro de campos, se necessário
                var nome = p.Nome?.Replace(";", ",") ?? "";
                var cat = p.Categoria?.Replace(";", ",") ?? "";
                sw.WriteLine($"{p.Id};{nome};{cat};{p.EstoqueMinimo};{p.Saldo}");
            }
        }

        // Replace (atômico o máximo possível): apaga o original e move o temp
        if (File.Exists(path)) File.Delete(path);
        File.Move(temp, path);
    }

    public static void SaveMovimentoAtomic(string path, IEnumerable<Movimento> movimentos)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        var dir = Path.GetDirectoryName(path) ?? ".";
        Directory.CreateDirectory(dir);

        string temp = Path.Combine(dir, Path.GetRandomFileName() + ".tmp");

        using (var sw = new StreamWriter(temp, false, new UTF8Encoding(false)))
        {
            sw.WriteLine("id;produtoId;tipo;quantidade;data;observacao");
            foreach (var p in movimentos.OrderBy(p => p.Id))
            {
                // Escape simples: substitui ; dentro de campos, se necessário                
                var tipo = p.Tipo?.Replace(";", ",") ?? "";
                var obs = p.Observacao?.Replace(";", ",") ?? "";
                sw.WriteLine($"{p.Id};{p.ProdutoId};{tipo};{p.Quantidade};{p.Data};{obs}");
            }
        }

        // Replace (atômico o máximo possível): apaga o original e move o temp
        if (File.Exists(path)) File.Delete(path);
        File.Move(temp, path);
    }
}

