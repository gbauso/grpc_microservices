{{/*
Expand the name of the chart.
*/}}
{{- define "grpc.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "grpc.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "grpc.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "grpc.labels" -}}
helm.sh/chart: {{ include "grpc.chart" . }}
{{ include "grpc.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "grpc.selectorLabels" -}}
app: {{ include "grpc.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "grpc.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "grpc.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{- define "grpc.initContainers" -}}
initContainers:
      - name: check-rabbitmq-ready
        image: busybox
        command: [ 'sh', '-c',
            'until wget http://{{ .Values.rabbitmq.user }}:{{ .Values.rabbitmq.password }}@{{ include "grpc.rabbitmq" . }}:15672/api/aliveness-test/%2F;
            do echo waiting for rabbitmq; sleep 2; done;' ]
        resources:
            limits:
              cpu: "50m"
              memory: "100Mi"
      - name: check-elk-ready
        image: busybox
        command: [ 'sh', '-c',
            'until wget http://{{ include "grpc.elastic" . }}:{{ .Values.elastic.port }};
            do echo waiting for {{ include "grpc.elastic" . }}; sleep 2; done;' ]
        resources:
            limits:
              cpu: "50m"
              memory: "100Mi"
{{- end }}

{{- define "grpc.metrics" -}}
- name: metrics
    targetPort: {{ .Values.metricsPort }} 
    port: {{ .Values.metricsPort }}
{{- end }}

{{- define "grpc.metrics.annotation" -}}
annotations:
    prometheus.io/scrape: true   
    prometheus.io/path: /metrics 
    prometheus.io/port: 3000
{{- end }}

{{- define "grpc.rabbitmq" -}}
{{- if .Values.rabbitmq.namespace }}
{{- printf "%s.%s" .Values.rabbitmq.host .Values.rabbitmq.namespace }}
{{- else }}
{{- .Values.rabbitmq.host }}
{{- end }}
{{- end }}

{{- define "grpc.etcd" -}}
{{- if .Values.etcd.namespace }}
{{- printf "%s.%s" .Values.etcd.host .Values.etcd.namespace }}
{{- else }}
{{- .Values.etcd.host }}
{{- end }}
{{- end }}

{{- define "grpc.fluentd" -}}
{{- if .Values.fluentd.namespace }}
{{- printf "%s.%s" .Values.fluentd.host .Values.fluentd.namespace }}
{{- else }}
{{- .Values.fluentd.host }}
{{- end }}
{{- end }}

{{- define "grpc.elastic" -}}
{{- if .Values.elastic.namespace }}
{{- printf "%s.%s" .Values.elastic.host .Values.elastic.namespace }}
{{- else }}
{{- .Values.elastic.host }}
{{- end }}
{{- end }}

{{- define "grpc.weatherService" -}}
{{- if .Values.weatherService.namespace }}
{{- printf "%s.%s" .Values.weatherService.host .Values.weatherService.namespace }}
{{- else }}
{{- .Values.weatherService.host }}
{{- end }}
{{- end }}

{{- define "grpc.populationService" -}}
{{- if .Values.populationService.namespace }}
{{- printf "%s.%s" .Values.populationService.host .Values.populationService.namespace }}
{{- else }}
{{- .Values.populationService.host }}
{{- end }}
{{- end }}

{{- define "grpc.discoveryService" -}}
{{- if .Values.discoveryService.namespace }}
{{- printf "%s.%s" .Values.discoveryService.host .Values.discoveryService.namespace }}
{{- else }}
{{- .Values.discoveryService.host }}
{{- end }}
{{- end }}

{{- define "grpc.nearbyCitiesService" -}}
{{- if .Values.nearbyCitiesService.namespace }}
{{- printf "%s.%s" .Values.nearbyCitiesService.host .Values.nearbyCitiesService.namespace }}
{{- else }}
{{- .Values.nearbyCitiesService.host }}
{{- end }}
{{- end }}
