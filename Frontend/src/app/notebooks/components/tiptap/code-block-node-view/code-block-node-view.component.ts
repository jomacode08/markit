import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, computed, OnDestroy, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AngularNodeViewComponent, TiptapNodeViewContentDirective } from 'ngx-tiptap';

import { Select, SelectModule } from 'primeng/select';

import { CODE_BLOCK_LANGUAGES } from '../../../extensions/code-block-lowlight';

interface CodeBlockLanguageOption {
  label: string;
  value: string | null;
}

@Component({
  selector: 'app-code-block-node-view',
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    TiptapNodeViewContentDirective
  ],
  templateUrl: './code-block-node-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeBlockNodeViewComponent extends AngularNodeViewComponent implements OnDestroy {
  protected readonly languages = CODE_BLOCK_LANGUAGES;
  protected readonly languageOptions: CodeBlockLanguageOption[] = [
    { label: 'Plain text', value: null },
    ...CODE_BLOCK_LANGUAGES.map((language) => ({
      label: language,
      value: language,
    })),
  ];
  protected readonly isEditingLanguage = signal<boolean>(false);
  protected readonly copied = signal<boolean>(false);
  protected readonly copyFailed = signal<boolean>(false);

  protected readonly language = computed<string | null>(() => {
    const language = this.node().attrs['language'];
    return typeof language === 'string' && language.length > 0 ? language : null;
  });

  protected readonly selectedLanguage = computed<string | null>(() => {
    const language = this.language();
    return language && this.languages.includes(language) ? language : null;
  });

  protected readonly languageLabel = computed<string>(() => {
    return this.language() ?? 'Plain text';
  });

  private copyTimeoutHandle: number | undefined;

  public ngOnDestroy(): void {
    if (this.copyTimeoutHandle) {
      window.clearTimeout(this.copyTimeoutHandle);
    }
  }

  public setLanguage(language: string | null): void {
    this.updateAttributes()({
      language,
    });
    this.closeLanguageSelector();
  }

  public openLanguageSelector(): void {
    this.isEditingLanguage.set(true);
  }

  public closeLanguageSelector(): void {
    this.isEditingLanguage.set(false);
  }

  public async copyCode(): Promise<void> {
    if (this.copyTimeoutHandle){
      window.clearTimeout(this.copyTimeoutHandle);
      this.copyTimeoutHandle = undefined;
    }

    this.copied.set(false);
    this.copyFailed.set(false);

    try {
      await navigator.clipboard.writeText(this.node().textContent);
      this.copied.set(true);
      this.copyTimeoutHandle = window.setTimeout(() => this.copied.set(false), 1400);
    } catch {
      this.copyFailed.set(true);
      this.copyTimeoutHandle = window.setTimeout(() => this.copyFailed.set(false), 1800);
    }
  }
}
