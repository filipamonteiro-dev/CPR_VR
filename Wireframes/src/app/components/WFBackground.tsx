import React from "react";

interface WFBackgroundProps {
  children: React.ReactNode;
  className?: string;
}

export function WFBackground({ children, className = "" }: WFBackgroundProps) {
  return (
    <div
      className={`relative w-full h-full overflow-hidden ${className}`}
      style={{ background: "#05080f" }}
    >
      {/* Perspective floor grid */}
      <div
        className="absolute inset-0 pointer-events-none"
        style={{
          backgroundImage: `
            linear-gradient(rgba(255,255,255,0.045) 1px, transparent 1px),
            linear-gradient(90deg, rgba(255,255,255,0.045) 1px, transparent 1px)
          `,
          backgroundSize: "60px 60px",
        }}
      />

      {/* Radial vignette — VR lens effect */}
      <div
        className="absolute inset-0 pointer-events-none"
        style={{
          background:
            "radial-gradient(ellipse 80% 80% at 50% 50%, transparent 35%, rgba(5,8,15,0.75) 100%)",
        }}
      />

      {/* Subtle horizontal horizon */}
      <div
        className="absolute inset-x-0 pointer-events-none"
        style={{ top: "48%", height: "1px", background: "rgba(255,255,255,0.04)" }}
      />

      {/* Corner tick marks */}
      <div className="absolute top-4 left-4 w-8 h-8 border-t border-l border-white/20 pointer-events-none" />
      <div className="absolute top-4 right-4 w-8 h-8 border-t border-r border-white/20 pointer-events-none" />
      <div className="absolute bottom-4 left-4 w-8 h-8 border-b border-l border-white/20 pointer-events-none" />
      <div className="absolute bottom-4 right-4 w-8 h-8 border-b border-r border-white/20 pointer-events-none" />

      {children}
    </div>
  );
}
