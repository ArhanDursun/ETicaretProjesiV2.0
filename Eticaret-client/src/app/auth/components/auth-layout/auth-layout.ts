import { Component, ElementRef, HostListener, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-auth-layout',
  standalone: false,
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.scss',
})
export class AuthLayout implements AfterViewInit, OnDestroy {
  @ViewChild('particleCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;
  
  private ctx!: CanvasRenderingContext2D;
  private particles: any[] = [];
  private animationId!: number;
  private mouse = { x: -1000, y: -1000 };

  constructor(private cdr: ChangeDetectorRef) {}

  ngAfterViewInit() {
    this.initCanvas();
    this.createParticles();
    this.animate();
    this.cdr.detectChanges();
  }

  ngOnDestroy() {
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
    }
  }

  @HostListener('window:resize')
  onResize() {
    this.initCanvas();
    this.createParticles();
  }

  @HostListener('mousemove', ['$event'])
  onMouseMove(event: MouseEvent) {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    this.mouse.x = event.clientX - rect.left;
    this.mouse.y = event.clientY - rect.top;
  }

  private initCanvas() {
    const canvas = this.canvasRef.nativeElement;
    this.ctx = canvas.getContext('2d') as CanvasRenderingContext2D;
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
  }

  private createParticles() {
    this.particles = [];
    const canvasWidth = this.canvasRef.nativeElement.width;
    const canvasHeight = this.canvasRef.nativeElement.height;
    const particleCount = Math.floor((canvasWidth * canvasHeight) / 8000); // Responsive density
    
    for (let i = 0; i < Math.min(particleCount, 100); i++) {
      this.particles.push({
        x: Math.random() * canvasWidth,
        y: Math.random() * canvasHeight,
        size: Math.random() * 1.5 + 1,
        speedX: Math.random() * 0.6 - 0.3,
        speedY: Math.random() * 0.6 - 0.3,
      });
    }
  }

  private animate() {
    if (!this.ctx) return;
    
    const canvasWidth = this.canvasRef.nativeElement.width;
    const canvasHeight = this.canvasRef.nativeElement.height;
    
    this.ctx.clearRect(0, 0, canvasWidth, canvasHeight);
    
    for (let i = 0; i < this.particles.length; i++) {
      const p = this.particles[i];

      // Move
      p.x += p.speedX;
      p.y += p.speedY;

      // Screen wrap
      if (p.x < 0) p.x = canvasWidth;
      if (p.x > canvasWidth) p.x = 0;
      if (p.y < 0) p.y = canvasHeight;
      if (p.y > canvasHeight) p.y = 0;

      // Draw particle
      this.ctx.beginPath();
      this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      this.ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
      this.ctx.fill();

      // Connect to mouse
      const mDx = this.mouse.x - p.x;
      const mDy = this.mouse.y - p.y;
      const mDistance = Math.sqrt(mDx * mDx + mDy * mDy);

      if (mDistance < 150) {
        // Attraction/Repulsion effect
        p.x += mDx * 0.01; 
        p.y += mDy * 0.01;

        this.ctx.beginPath();
        this.ctx.strokeStyle = `rgba(255, 255, 255, ${0.3 - mDistance / 150})`;
        this.ctx.lineWidth = 0.6;
        this.ctx.moveTo(p.x, p.y);
        this.ctx.lineTo(this.mouse.x, this.mouse.y);
        this.ctx.stroke();
      }

      // Connect to other particles
      for (let j = i + 1; j < this.particles.length; j++) {
        const p2 = this.particles[j];
        const dx = p.x - p2.x;
        const dy = p.y - p2.y;
        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance < 100) {
          this.ctx.beginPath();
          this.ctx.strokeStyle = `rgba(255, 255, 255, ${0.15 - distance / 100})`;
          this.ctx.lineWidth = 0.4;
          this.ctx.moveTo(p.x, p.y);
          this.ctx.lineTo(p2.x, p2.y);
          this.ctx.stroke();
        }
      }
    }

    this.animationId = requestAnimationFrame(() => this.animate());
  }
}
