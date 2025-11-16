"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { StatCard } from "@/components/dashboard/stat-card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Search,
  Package,
  TrendingUp,
  Users,
  Youtube,
  Loader2,
} from "lucide-react";
import { analysisApi } from "@/lib/api";
import { toast } from "sonner";

export default function DashboardPage() {
  const [keyword, setKeyword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const router = useRouter();

  const handleSearch = async () => {
    setIsLoading(true);
    try {
      const result = await analysisApi.run({ keyword: keyword.trim() });

      console.log("Resposta da API:", result);

      // A API retorna um array diretamente, não um objeto com propriedade products
      const products = Array.isArray(result) ? result : result.products || [];

      console.log("Produtos recebidos:", products);
      console.log("Quantidade de produtos:", products.length);

      // Salvar produtos no localStorage
      if (products && products.length > 0) {
        localStorage.setItem("products", JSON.stringify(products));
        console.log("Produtos salvos no localStorage");
        toast.success(
          `Análise concluída! ${products.length} produtos encontrados`
        );
      } else {
        console.log("Nenhum produto para salvar");
        toast.warning("Nenhum produto encontrado para esta busca");
      }

      router.push(`/produtos`);
    } catch (error) {
      console.error("Erro ao analisar produtos:", error);
      toast.error("Erro ao analisar produtos. Tente novamente.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !isLoading) {
      handleSearch();
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-8">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
            <p className="text-muted-foreground">
              Encontre produtos lucrativos do AliExpress para vender no Mercado
              Livre
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Pesquisar Produtos</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex gap-4">
                <Input
                  placeholder="Digite uma palavra-chave (opcional - deixe vazio para produtos em alta)..."
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  onKeyDown={handleKeyPress}
                  disabled={isLoading}
                  className="flex-1"
                />
                <Button
                  onClick={handleSearch}
                  disabled={isLoading}
                  className="gap-2"
                >
                  {isLoading ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Analisando...
                    </>
                  ) : (
                    <>
                      <Search className="h-4 w-4" />
                      {keyword.trim()
                        ? "Pesquisar produtos"
                        : "Ver produtos em alta"}
                    </>
                  )}
                </Button>
              </div>
              <p className="text-sm text-muted-foreground mt-2">
                {keyword.trim()
                  ? "Busca produtos no AliExpress e calcula score, margem e competição"
                  : "Deixe vazio para descobrir os produtos mais quentes do momento"}
              </p>
            </CardContent>
          </Card>

          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
            <StatCard
              title="Produtos Analisados"
              value="1,248"
              icon={Package}
              description="Última semana"
            />
            <StatCard
              title="Margem Média Potencial"
              value="65.8%"
              icon={TrendingUp}
              description="+4.2% vs mês anterior"
            />
            <StatCard
              title="Concorrência Média ML"
              value="Média"
              icon={Users}
              description="Baseado em 15 produtos"
            />
            <StatCard
              title="Engajamento YouTube"
              value="82%"
              icon={Youtube}
              description="Sentimento positivo"
            />
          </div>

          <div className="grid gap-6 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle>Produtos em Alta</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {[
                    { name: "Smartwatches", growth: "+24%" },
                    { name: "Fones Bluetooth", growth: "+18%" },
                    { name: "Fitas LED", growth: "+15%" },
                    { name: "Suportes Veiculares", growth: "+12%" },
                  ].map((item, i) => (
                    <div key={i} className="flex justify-between items-center">
                      <span className="text-sm font-medium">{item.name}</span>
                      <span className="text-sm text-green-600 font-semibold">
                        {item.growth}
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Últimas Análises</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {[
                    { product: "Smartwatch Fitness Pro X1", score: 92 },
                    { product: "Kit Elásticos Treino", score: 93 },
                    { product: "Fita LED RGB 5m Smart", score: 95 },
                    { product: "Tapete Yoga Premium", score: 90 },
                  ].map((item, i) => (
                    <div key={i} className="flex justify-between items-center">
                      <span className="text-sm font-medium">
                        {item.product}
                      </span>
                      <span className="text-sm font-bold text-primary">
                        Score: {item.score}
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </div>
  );
}
