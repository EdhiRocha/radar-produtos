"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Home, List, Settings, BarChart3 } from "lucide-react";
import { cn } from "@/lib/utils";

const menuItems = [
  { icon: Home, label: "Dashboard", href: "/" },
  { icon: List, label: "Produtos", href: "/produtos" },
  { icon: BarChart3, label: "Análises", href: "/analises" },
  { icon: Settings, label: "Configurações", href: "/configuracoes" },
];

export function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="fixed left-0 top-0 h-full w-64 border-r border-border bg-card">
      <div className="flex h-16 items-center border-b border-border px-6">
        <div className="flex flex-col">
          <h1 className="text-xl font-bold text-primary">AliHunter</h1>
          <p className="text-xs text-muted-foreground">Radar de Produtos</p>
        </div>
      </div>
      <nav className="space-y-1 p-4">
        {menuItems.map((item) => {
          const Icon = item.icon;
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-4 py-3 text-sm font-medium transition-colors",
                isActive
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              )}
            >
              <Icon className="h-5 w-5" />
              {item.label}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
