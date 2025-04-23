import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PresavePanelComponent } from './presave-panel.component';

describe('PresavePanelComponent', () => {
  let component: PresavePanelComponent;
  let fixture: ComponentFixture<PresavePanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PresavePanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PresavePanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
