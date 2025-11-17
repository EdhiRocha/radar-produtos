"use client";

import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { StatCard } from "@/components/dashboard/stat-card";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Package, TrendingUp, Users, Youtube } from "lucide-react";

export default function DashboardPage() {
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
