import { Sidebar } from "@/components/layout/sidebar";
import { Header } from "@/components/layout/header";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { BarChart3 } from 'lucide-react';

export default function AnalysisPage() {
  return (
    <div className="min-h-screen bg-background">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-16">
        <div className="p-8 space-y-6">
          <div>
            <h2 className="text-3xl font-bold tracking-tight">Análises</h2>
            <p className="text-muted-foreground">
              Visualize tendências e insights de produtos
            </p>
          </div>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BarChart3 className="h-5 w-5" />
                Painel de Análises
              </CardTitle>
            </CardHeader>
            <CardContent className="flex items-center justify-center h-64">
              <p className="text-muted-foreground">
                Gráficos e análises detalhadas em breve...
              </p>
            </CardContent>
          </Card>
        </div>
      </main>
    </div>
  );
}
